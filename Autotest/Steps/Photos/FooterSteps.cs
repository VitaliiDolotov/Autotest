using TechTalk.SpecFlow;

namespace Autotest.Steps.Photos
{
    [Binding]
    public class FooterSteps
    {
        [When(@"User clicks Privacy Policy button in the footer on the page")]
        public void WhenUserClicksPrivacyPolicyButtonInTheFooterOnThePage()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
