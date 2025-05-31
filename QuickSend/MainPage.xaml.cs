using System.Threading.Tasks;

namespace QuickSend {
    public partial class MainPage : ContentPage {

        public MainPage() {
            InitializeComponent();
        }

        private async void QuickSendButton_Clicked(object sender, EventArgs e) {
            FileResult? fileResult = await FilePicker.PickAsync().ConfigureAwait(false);
            if (fileResult != null) {
                using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
                byte[] data =new byte[1024];
                int dataLength = await stream.ReadAsync(data).ConfigureAwait(false);
            }
        }
    }
}
