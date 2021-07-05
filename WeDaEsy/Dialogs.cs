using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace WeDaESy
{
    internal class Dialogs
    {
        internal static async Task DisplayVPCodeWarningDialogAsync(object sender)
        {
            ContentDialog VPCodeDialog = new ContentDialog()
            {
                Name = "VP-Code Dialog",
                Title = "VP-Code fehlt!",
                PrimaryButtonText = "Speichern",
                DefaultButton = ContentDialogButton.Primary,
            };
            StackPanel DialogStack = new StackPanel
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
            };
            TextBlock textBlockDialog = new TextBlock
            {
                Text = "Sie haben noch keinen VP-Code für den aktuellen Vorgang eingegeben. " + Environment.NewLine + "Bitte holen Sie das nun nach: "
            };
            TextBox txt_input_VPCode1 = new TextBox
            {
                Header = "VP-Code:",
                PlaceholderText = "VP-Code hier eingeben"
            };
            VPCodeDialog.PrimaryButtonClick += SaveVPCode_Click;
            DialogStack.Children.Add(textBlockDialog);
            DialogStack.Children.Add(txt_input_VPCode1);
            VPCodeDialog.Content = DialogStack;
            
            await VPCodeDialog.ShowAsync();
            

            void SaveVPCode_Click(object Sender, ContentDialogButtonClickEventArgs args)
            {
                // Set the sender's VPCode as the input 
                // TODO check against empty inputs e.g. with firstly disabled button, enable when txt_input_VPCode1 TextChanged fires
                if(sender is BIT bit) bit.VPCode = txt_input_VPCode1.Text;
                if(sender is BFI10 bFI) bFI.VPCode = txt_input_VPCode1.Text;
                if (sender is Demograph demograph) demograph.VPCode = txt_input_VPCode1.Text;
            }
        }
        /// <summary>
        /// Opens a dialog-Box containing arbitrary information
        /// </summary>
        /// <param name="info">The Information to be displayed</param>
        /// <returns></returns>
        internal static async Task DisplayInfoBoxAsync(string info)
        {
            ContentDialog InfoDialog = new ContentDialog()
            {
                Name = "InfoDialog",
                Title = "Information",
                PrimaryButtonText = "Schließen",
                DefaultButton = ContentDialogButton.Primary
            };
            StackPanel DialogStack = new StackPanel
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
            };
            TextBlock textBlockDialog = new TextBlock
            {
                Text = info
            };
            DialogStack.Children.Add(textBlockDialog);
            InfoDialog.Content = DialogStack;
            InfoDialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) => { sender.Hide(); };
            await InfoDialog.ShowAsync();
        }
    }
}