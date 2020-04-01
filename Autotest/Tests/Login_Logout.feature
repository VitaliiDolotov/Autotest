Feature: Login_Logout
	Check that user can be successfully
	sign in and sign out from the website

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user

@Regression @ProductionSmoke
Scenario: CheckThatLoginAndLogOutWorksCorrectly
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in
	And Correct menu items are displayed in the User Profile Menu
	When User has logged out
	Then User has successfully logged out

@Regression @ProductionSmoke
Scenario: CheckThatUserCanLoginUsingFaceBookAccount
	When User login using facebook account
	Then User has successfully logged in
	And Correct menu items are displayed in the User Profile Menu
	When User has logged out
	Then User has successfully logged out

@Regression @ProductionSmoke
Scenario: CheckThatUserCanLoginUsingGoogleAccount
	When User login using google account
	Then User has successfully logged in
	And Correct menu items are displayed in the User Profile Menu
	When User has logged out
	Then User has successfully logged out

@Regression @ProductionSmoke
Scenario: CheckThatUserCanLoginUsingTwitterAccount
	When User login using twitter account
	Then User has successfully logged in
	And Correct menu items are displayed in the User Profile Menu
	When User has logged out
	Then User has successfully logged out