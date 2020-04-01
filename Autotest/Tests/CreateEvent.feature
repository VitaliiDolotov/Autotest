Feature: CreateEvent
	Check that all features on the Create event page works fine

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in

@Regression @ProductionSmoke
Scenario: Check that events are displayed on the  Create event page
	When User clicks on the 'Events' button on the header
	And User clicks on the 'Create event' item in Events menu
	Then User sees 'Create event' page title
	When User provides the 'testAuto11' Event name, 'Auto test 11' Event description and clicks on the Get started FREE button
	Then User sees 'testAuto11' event title on My event page	
	