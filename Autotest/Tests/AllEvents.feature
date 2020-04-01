Feature: AllEvents
	Check that all features on the All events page works fine

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in

@Regression
Scenario: Check that events are displayed on the All events page
	When User clicks on the 'Events' button on the header
	And User clicks on the 'All events' item in Events menu
	Then User sees 'CrowdPic Events:' page title
	And User sees list of events:
	| eventName           |
	| test32              |
	| test update from AP |
	| test create22       |
	| test34              |

@Regression
Scenario: Check filtration on the All events page
	When User clicks on the 'Events' button on the header
	And User clicks on the 'All events' item in Events menu
	When User enters 'test223' to the search string
	Then User sees 'CrowdPic Events:' page title
	And User sees list of filtered events:
	| test223             |

@Regression
Scenario: Check that selected event is opened
	When User clicks on the 'Events' button on the header
	And User clicks on the 'All events' item in Events menu
	When User clicks on the 'test32' on the All events
	Then User sees 'test32' event title on the Event page
	And User sees '2 photos uploaded 2 March 2018' in the amount and date fields of Event page
	And User sees '2' pictures are displayed on Event page