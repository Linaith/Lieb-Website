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

        public async Task<int> CreatePoll(string question, List<string> options, int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                            .Include(r => r.SignUps)
                            .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null) return 0;
            List<ulong> users = raid.SignUps.Where(s => s.LiebUserId != null).Select(s => (ulong)s.LiebUserId).ToList();
            return await CreatePoll(question, options, users, raidId);
        }

        public async Task<int> CreatePoll(string question, List<string> options, List<ulong> users, int? raidId = null)
        {
            Poll poll = new Poll()
            {
                Question = question,
                RaidId = raidId
            };
            foreach(string option in options)
            {
                poll.Options.Add(new PollOption()
                {
                    Name = option
                });
            }
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

        public async Task UpdateAnswer(int pollId, int pollOptionId, ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            Poll? poll = context.Polls
                            .Include(p => p.Answers)
                            .FirstOrDefault(p => p.PollId == pollId && p.Answers.Where(a => a.UserId == userId).Any());

            if (poll == null) return;

            PollAnswer answer = poll.Answers.First(a => a.UserId == userId);
            answer.PollOptionId = pollOptionId;
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
    }
}
