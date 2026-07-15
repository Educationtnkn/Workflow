using Microsoft.EntityFrameworkCore;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//public class WorkflowLogDbContext : DbContext
//{
//    public WorkflowLogDbContext(DbContextOptions<WorkflowLogDbContext> options)
//        : base(options) { }

//    public DbSet<WorkflowActionLog> WorkflowActivityLogs => Set<WorkflowActionLog>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<WorkflowActionLog>(entity =>
//        {
//            entity.ToTable("WorkflowActivityLogs", "logs"); // schema: logs, table: WorkflowActivityLogs 

//            entity.HasKey(e => e.Id);

//            entity.Property(e => e.WorkflowInstanceId).HasMaxLength(100).IsRequired();
//            entity.Property(e => e.ActivityId).HasMaxLength(100).IsRequired();
//            entity.Property(e => e.ActivityName).HasMaxLength(200);
//            entity.Property(e => e.ActivityType).HasMaxLength(200);
//            entity.Property(e => e.Status).HasMaxLength(50);
//            entity.Property(e => e.BusinessMessage).HasMaxLength(500);
//            entity.Property(e => e.TenantId).HasMaxLength(100);
//        });
//    }
//}