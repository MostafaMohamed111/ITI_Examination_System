
using ITI.DAL.Entities.DomainEntities;
using Microsoft.EntityFrameworkCore;

namespace ITI.DAL.Data.AppDbContext;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<StudCr> StudCrs { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=MOSTAFAELBEBANY\\SQLEXPRESS;Database=ITI_Examination_System;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branch__3214EC07E298C240");

            entity.ToTable("Branch");

            entity.Property(e => e.Hotline)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MgrId).HasColumnName("Mgr_Id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Mgr).WithMany(p => p.Branches)
                .HasForeignKey(d => d.MgrId)
                .HasConstraintName("FK_Branch_Manager");

            entity.HasMany(d => d.IdTracks).WithMany(p => p.IdBranches)
                .UsingEntity<Dictionary<string, object>>(
                    "BranchTrack",
                    r => r.HasOne<Track>().WithMany()
                        .HasForeignKey("IdTrack")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BT_Track"),
                    l => l.HasOne<Branch>().WithMany()
                        .HasForeignKey("IdBranch")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BT_Branch"),
                    j =>
                    {
                        j.HasKey("IdBranch", "IdTrack");
                        j.ToTable("Branch_Track");
                        j.IndexerProperty<int>("IdBranch").HasColumnName("Id_Branch");
                        j.IndexerProperty<int>("IdTrack").HasColumnName("Id_Track");
                    });
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => new { e.QuestionId, e.Choice1 });

            entity.ToTable("Choice");

            entity.Property(e => e.QuestionId).HasColumnName("Question_Id");
            entity.Property(e => e.Choice1)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Choice");

            entity.HasOne(d => d.Question).WithMany(p => p.Choices)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Choice_Question");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Course__3214EC07161552B0");

            entity.ToTable("Course");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TotDegree).HasColumnName("Tot_Degree");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exam__3214EC07A061C723");

            entity.ToTable("Exam");

            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.MinDegree).HasColumnName("Min_Degree");
            entity.Property(e => e.QuestionNum).HasColumnName("Question_Num");
            entity.Property(e => e.TotDegree).HasColumnName("Tot_Degree");

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Exam_Course");

            entity.HasMany(d => d.IdQuests).WithMany(p => p.IdExams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamQuestion",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("IdQuest")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EQ_Question"),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("IdExam")
                        .HasConstraintName("FK_EQ_Exam"),
                    j =>
                    {
                        j.HasKey("IdExam", "IdQuest");
                        j.ToTable("Exam_Question");
                        j.IndexerProperty<int>("IdExam").HasColumnName("Id_Exam");
                        j.IndexerProperty<int>("IdQuest").HasColumnName("Id_Quest");
                    });
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Instruct__3214EC07204E591C");

            entity.ToTable("Instructor");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Degree)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasMany(d => d.Crs).WithMany(p => p.Ins)
                .UsingEntity<Dictionary<string, object>>(
                    "InstructorCourse",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CrsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_IC_Course"),
                    l => l.HasOne<Instructor>().WithMany()
                        .HasForeignKey("InsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_IC_Instructor"),
                    j =>
                    {
                        j.HasKey("InsId", "CrsId");
                        j.ToTable("Instructor_Course");
                        j.IndexerProperty<int>("InsId").HasColumnName("Ins_Id");
                        j.IndexerProperty<int>("CrsId").HasColumnName("Crs_Id");
                    });
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07B7942D80");

            entity.ToTable("Question");

            entity.Property(e => e.Body)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CorrectAns)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Correct_Ans");
            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Question_Course");
        });

        modelBuilder.Entity<StudCr>(entity =>
        {
            entity.HasKey(e => new { e.StdId, e.CrsId });

            entity.ToTable("Stud_Crs");

            entity.Property(e => e.StdId).HasColumnName("Std_Id");
            entity.Property(e => e.CrsId).HasColumnName("Crs_Id");

            entity.HasOne(d => d.Crs).WithMany(p => p.StudCrs)
                .HasForeignKey(d => d.CrsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SC_Course");

            entity.HasOne(d => d.Std).WithMany(p => p.StudCrs)
                .HasForeignKey(d => d.StdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SC_Student");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Student__3214EC072C14AF4C");

            entity.ToTable("Student");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Degree)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GraduationDate).HasColumnName("Graduation_Date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrackId).HasColumnName("Track_Id");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Track).WithMany(p => p.Students)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("FK_Student_Track");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Track__3214EC07F78913C7");

            entity.ToTable("Track");

            entity.Property(e => e.InsId).HasColumnName("Ins_Id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Program)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Ins).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.InsId)
                .HasConstraintName("FK_Track_Instructor");

            entity.HasMany(d => d.Crs).WithMany(p => p.Tracks)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackCourse",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CrsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TC_Course"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TC_Track"),
                    j =>
                    {
                        j.HasKey("TrackId", "CrsId");
                        j.ToTable("Track_Course");
                        j.IndexerProperty<int>("TrackId").HasColumnName("Track_Id");
                        j.IndexerProperty<int>("CrsId").HasColumnName("Crs_Id");
                    });

            entity.HasMany(d => d.InsNavigation).WithMany(p => p.TracksNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackIn",
                    r => r.HasOne<Instructor>().WithMany()
                        .HasForeignKey("InsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TI_Instructor"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TI_Track"),
                    j =>
                    {
                        j.HasKey("TrackId", "InsId");
                        j.ToTable("Track_Ins");
                        j.IndexerProperty<int>("TrackId").HasColumnName("Track_Id");
                        j.IndexerProperty<int>("InsId").HasColumnName("Ins_Id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
