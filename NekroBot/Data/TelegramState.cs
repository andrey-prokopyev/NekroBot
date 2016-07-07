namespace NekroBot.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TelegramState")]
    public partial class TelegramState
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChatId { get; set; }
    }
}
