Feature: Footer
	Check that  footer on the page works fine

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in

@Regression
Scenario: Check that user can open Privacy Policy page by link on footer
	When User clicks Privacy Policy button in the footer on the page
	Then User sees 'Privacy Policy' page title

@Regression
Scenario: Check that user can open Terms of Use page by link on footer
	When User clicks Terms of Use button in the footer on the page
	Then User sees 'Website Terms of Use' page title