﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataObjects.EF
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VTVChannelBrandingEntities : DbContext
    {
        public VTVChannelBrandingEntities()
            : base("name=VTVChannelBrandingEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ChannelEntity> ChannelEntities { get; set; }
        public virtual DbSet<TrafficEventEntity> TrafficEventEntities { get; set; }
    }
}