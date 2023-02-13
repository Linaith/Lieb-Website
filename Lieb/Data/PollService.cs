using Lieb.Models.GuildWars2.Raid;
using Lieb.Models.Poll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Lieb.Data
{
    public class PollService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;
        public PollService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
        }

        public List<Poll> GetPolls()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Polls
                            .Include(p => p.Options)
                            .Include(p => p.Answers)
                            .ToList();
        }

        public Poll GetPoll(int pollId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Options)
                            .Include(p => p.Answers)
                            .FirstOrDefault(p => p.PollId == pollId);

            if (poll != null) return poll;

            return new Poll();
        }

        public List<Poll> GetPollsByRaidId(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Polls
                            .Include(p => p.Options)
                            .Include(p => p.Answers)
                            .Where(p => p.RaidId == raidId).ToList();
        }

        public async Task<int> CreatePoll(string question, List<string> options, int raidId, bool isAutoPoll = false)
        {
            Poll poll = new Poll()
            {
                Question = question,
                RaidId = raidId,
                AllowCustomAnswer = false,
                AnswerType = AnswerType.Buttons
            };
            foreach(string option in options)
            {
                poll.Options.Add(new PollOption()
                {
                    Name = option
                });
            }
            return await CreatePoll(poll, raidId, isAutoPoll);
        }

        public async Task<int> CreatePoll(Poll poll, int raidId, bool isAutoPoll = false)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                            .Include(r => r.SignUps)
                            .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null) return 0;
            poll.Question = $"{raid.Title}: {poll.Question}";
            HashSet<ulong> users = raid.SignUps.Where(s => s.LiebUserId != null && s.IsMessageSignUp).Select(s => (ulong)s.LiebUserId).ToHashSet();
            return await CreatePoll(poll, users, raidId, isAutoPoll);
        }

        public async Task<int> CreatePoll(Poll poll, HashSet<ulong> users, int? raidId = null, bool isAutoPoll = false)
        {
            poll.RaidId = raidId;
            poll.IsAutoPoll = isAutoPoll;

            foreach(ulong user in users)
            {
                poll.Answers.Add(new PollAnswer()
                {
                    UserId = user
                });
            }

            using var context = _contextFactory.CreateDbContext();
            context.Polls.Add(poll);
            await context.SaveChangesAsync();
            await _discordService.SendPoll(poll);
            return poll.PollId;
        }

        public async Task DeletePoll(int pollId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Options)
                            .Include(p => p.Answers)
                            .FirstOrDefault(p => p.PollId == pollId);

            if(poll == null) return; 

            context.PollOptions.RemoveRange(poll.Options);
            context.PollAnswers.RemoveRange(poll.Answers);
            await context.SaveChangesAsync();

            poll.Options.Clear();
            poll.Answers.Clear();
            context.Polls.Remove(poll);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAnswer(int pollId, int pollOptionId, string answer, ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Answers)
                            .Include(p => p.Options)
                            .FirstOrDefault(p => p.PollId == pollId && p.Answers.Where(a => a.UserId == userId).Any());

            if (poll == null) return;

            PollAnswer pollAnswer = poll.Answers.First(a => a.UserId == userId);
            if(string.IsNullOrEmpty(answer) && pollOptionId > 0)
            {
                PollOption option = poll.Options.FirstOrDefault(o => o.PollOptionId == pollOptionId);
                answer = option != null ? option.Name : string.Empty;
            }

            pollAnswer.Answer = answer;
            pollAnswer.PollOptionId = pollOptionId;
            await context.SaveChangesAsync();
        }

        public async Task AddUser(int pollId, ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Answers)
                            .FirstOrDefault(p => p.PollId == pollId && p.Answers.Where(a => a.UserId == userId).Any());

            if (poll == null) return;

            poll.Answers.Add(new PollAnswer()
            {
                UserId = userId
            });
            await context.SaveChangesAsync();
            await _discordService.SendPoll(poll, new List<ulong>(){userId});
        }

        public async Task RemoveUser(int pollId, ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Answers)
                            .FirstOrDefault(p => p.PollId == pollId && p.Answers.Where(a => a.UserId == userId).Any());

            if (poll == null) return;

            PollAnswer answer = poll.Answers.First(a => a.UserId == userId);
            context.Remove(answer);
            await context.SaveChangesAsync();
            poll.Answers.Remove(answer);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAutoPolls()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Poll> polls = context.Polls.ToList();

            foreach(Poll poll in polls)
            {
                if((poll.IsAutoPoll && poll.CreatedAt < DateTime.UtcNow.AddDays(-7)))
                {
                    await DeletePoll(poll.PollId);
                }
            }
        }
    }
}
