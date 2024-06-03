using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MessengerClientMaui.Domain.Entities
{
    public class Message : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public string? nickname { get; set; }
        public int chatId { get; set; }

        private List<Reaction> user_reactions = new List<Reaction>();

        public List<Reaction> userReactions
        {
            get => user_reactions;
            set
            {
                user_reactions = value;
                OnPropertyChanged();
            }
        }
        private string? text { get; set; }
        public string? Text {
            get => text;
            set 
            {
                text = value;
                OnPropertyChanged();
            }
        }
        public DateTime date { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
