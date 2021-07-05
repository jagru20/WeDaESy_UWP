using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace WeDaESy
{
    /// <summary>
    /// Page to obtain demographical information
    /// </summary>
    public sealed partial class Demograph : Page
    {
        #region Public Properties

        /// <summary>
        /// Object to store the given answers of the demographic questionnaire
        /// </summary>
        public DemographicAnswers DemographicAnswers { get; }
        public string VPCode
        {
            get => vPCode;
            set
            {
                vPCode = value;
                try { DemographicAnswers.VPCode = value; }
                catch (Exception ex)
                { _ = Dialogs.DisplayInfoBoxAsync(ex.Message); }
            }
        }

        private string vPCode;

        #endregion Public Properties

        #region Public Constructors

        ///<summary>
        ///Constructor
        ///</summary>
        public Demograph()
        {

            DemographicAnswers = new DemographicAnswers();
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods
        /// <summary>
        /// Prompts the user to provide a VpCode if none is provided yet
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MainPage.VPCode == "")
            {//make sure there is a VPCode
                _ = Dialogs.DisplayVPCodeWarningDialogAsync(this);
            }
            else
            {
                DemographicAnswers.VPCode = MainPage.VPCode;
            }
            base.OnNavigatedTo(e);
        }
        ///<summary>
        /// Return to previous page
        ///</summary>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton CurrentButton)
            {
                //store answer in demographicAnswers object
                DemographicAnswers[CurrentButton.GroupName] = CurrentButton.Content;
            }
        }

        private void Txt_Branch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                DemographicAnswers["Branch"] = textBox.Text;
            }
        }

        #endregion Private Methods

        private void Btn_Demograph_finished_Click(object sender, RoutedEventArgs e)
        {
            HelperQuestionPages.LogResults((JObject)JToken.FromObject(DemographicAnswers), "Demographics");
            Frame.Navigate(typeof(MainPage), new DemographicNavParams() { ObtainedVpCode = DemographicAnswers.VPCode, SkillLevel = DemographicAnswers.Skill });
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            DemographicAnswers.Skill = Slider_Skill.Value;
        }
    }
    /// <summary>
    /// Provides NavigationParameters 
    /// </summary>
    public class DemographicNavParams
    {
        /// <summary>
        /// Obtained Vp-Code
        /// </summary>
        public string ObtainedVpCode { get; set; }

        /// <summary>
        /// Obtained skill-level in building with wooden bricks
        /// </summary>
        public double SkillLevel { get; set; }
    }

    /// <summary>
    /// Stores the Answers to the questions on Demograph.xaml
    /// </summary>
    public class DemographicAnswers : SurveyResultsObject
    {


        #region Public Properties
        /// <summary>
        /// should equal <see cref="MainPage.VPCode"/>
        /// </summary>
        public string VPCode { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public string Age { get; set; }

        /// <summary>
        /// Branch, the VP is employed at
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Employment status
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Educational qualification
        /// </summary>
        public string Quali { get; set; }

        /// <summary>
        /// Skill to build with kapla stones
        /// </summary>
        public double Skill { get; set; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Makes the properties of this class available via [] operator
        /// </summary>
        public object this[string name]
        {
            get
            {
                PropertyInfo property = typeof(DemographicAnswers).GetProperty(name);

                if (property.Name == name && property.CanRead)
                {
                    return property.GetValue(this, null);
                }

                throw new ArgumentException("Can't find property");
            }
            set
            {
                try
                {
                    PropertyInfo myPropInfo = typeof(DemographicAnswers).GetProperty(name);
                    myPropInfo.SetValue(this, value, null);
                }
                catch (ArgumentException ex)
                {
                    _ = Dialogs.DisplayInfoBoxAsync(ex.Message);
                }
            }
        }

        #endregion Public Indexers
    }
}