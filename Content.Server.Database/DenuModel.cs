using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

public static class DenuModel
{
    [Table("denu_settings")]
    public sealed class DenuSettings
    {
        [Key]
        [Column("player_user_id")]
        public Guid PlayerUserId { get; set; }

        [Column("settings", TypeName = "jsonb")]
        public string SettingsJson { get; set; } = "{}";

        public Player Player { get; set; } = null!;
    }
}
