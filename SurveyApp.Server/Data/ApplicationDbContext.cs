using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SurveyApp.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();
        public DbSet<QuestionBankOption> QuestionBankOptions => Set<QuestionBankOption>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserProfile
            builder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100);

                entity.Property(x => x.LastName)
                    .HasMaxLength(100);

                entity.Property(x => x.Gender)
                    .HasMaxLength(50);

                entity.Property(x => x.City)
                    .HasMaxLength(100);

                entity.HasOne(x => x.User)
                    .WithOne(x => x.UserProfile)
                    .HasForeignKey<UserProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.PhotoContentType)
                    .HasMaxLength(100);

                entity.HasOne(x => x.User)
                    .WithOne(x => x.UserProfile)
                    .HasForeignKey<UserProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });

            // Survey
            builder.Entity<Survey>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .HasMaxLength(2000);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany(x => x.CreatedSurveys)
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Question
            builder.Entity<Question>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Text)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(x => x.QuestionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(x => x.Survey)
                    .WithMany(x => x.Questions)
                    .HasForeignKey(x => x.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // QuestionOption
            builder.Entity<QuestionOption>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Text)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Question)
                    .WithMany(x => x.Options)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SurveyResponse
            builder.Entity<SurveyResponse>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Survey)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.SurveyResponses)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Answer
            builder.Entity<Answer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TextAnswer)
                    .HasMaxLength(4000);

                entity.HasOne(x => x.SurveyResponse)
                    .WithMany(x => x.Answers)
                    .HasForeignKey(x => x.SurveyResponseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Question)
                    .WithMany(x => x.Answers)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AnswerOption
            builder.Entity<AnswerOption>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Answer)
                    .WithMany(x => x.AnswerOptions)
                    .HasForeignKey(x => x.AnswerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.QuestionOption)
                    .WithMany(x => x.AnswerOptions)
                    .HasForeignKey(x => x.QuestionOptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<QuestionBankItem>(entity =>
            {
                entity.Property(q => q.Text)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(q => q.QuestionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(q => q.Category)
                    .HasMaxLength(200);

                entity.HasMany(q => q.Options)
                    .WithOne(o => o.QuestionBankItem)
                    .HasForeignKey(o => o.QuestionBankItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<QuestionBankOption>(entity =>
            {
                entity.Property(o => o.Text)
                    .IsRequired()
                    .HasMaxLength(500);
            });
        }
    }
}