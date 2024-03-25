using System.ComponentModel;

namespace ActivityTracker.src.util;

public interface IBotConfig : INotifyPropertyChanged {
    int Id { get; set;  }
    
    string HypixelGuildId { get; set; }
    
    string HypixelApiKey { get; set; }
    
    [DefaultValue(1)]
    int FetchRateMinutes { get; set; }
    
    IEnumerable<string> ExtraPlayers { get; }
}