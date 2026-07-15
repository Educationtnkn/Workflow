using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseWorkflow.Storage.Context
{
    //public class WorkflowDbContext : DbContext
    //{
    //    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

    //    public DbSet<Workflowdefinition> WorkflowDefinitions => Set<Workflowdefinition>();

    //    public DbSet<WorkflowVersion> WorkflowVersions => Set<WorkflowVersion>();
    //    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    //    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    //    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();


    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
             

    //        modelBuilder.Entity<WorkflowStep>()
    //            .HasOne<Workflowdefinition>()
    //            .WithMany(d => d.Steps)
    //            .HasForeignKey(s => s.WorkflowDefinitionId);

    //        // ... other relationships
    //    }
    //}
}
