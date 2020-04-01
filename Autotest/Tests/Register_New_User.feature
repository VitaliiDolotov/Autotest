Feature: Register_New_User
	Check that new user
	can be successfully registered

@Regression @ProductionSmoke @Delete_Newly_Created_User
Scenario: RegisterNewUser
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User has registered new account
	And User provides the Login and Password and clicks on the login button
	Then User has successfully logged in
	And Correct menu items are displayed in the User Profile Menu
	When User has logged out
	Then User has successfully logged out
