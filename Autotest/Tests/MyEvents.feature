Feature: MyEvents
	Check that all features on the My events page works as expected

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in

@Regression @ProductionSmoke
Scenario: Check that events are displayed on the My events page
	When User clicks on the 'Events' button on the header
	And User clicks on the 'My events' item in Events menu
	Then User sees 'My Events:' page title
	And User sees list of events on My events page:
	| eventName     |
	| testJ1        |
	| testAuto10 |

@Regression
Scenario: Check filtration on My events page
	When User clicks on the 'Events' button on the header
	And User clicks on the 'My events' item in Events menu
	Then User sees 'My Events:' page title
	When User types 'testJ1' on search field on the My events page
	Then User sees list of filtered events on My events page:
	| eventName     |
	| testJ1        | 

@Regression @ProductionSmoke
Scenario: Check that user can select event on My events page and open it
	When User clicks on the 'Events' button on the header
	And User clicks on the 'My events' item in Events menu
	Then User sees 'My Events:' page title
	When User types 'Ylia' on search field on the My events page
	When User open 'Ylia' event on the My events page
	Then User sees 'Ylia' event title on My event page
	And User sees event info on My event page:
	| eventInfo          |
	| Ylia's event       |
	| Tier: Get Started  |
	| Up to 500 photos   |
	| 15 March 2018      |
	And User sees '2' in Total uploaded photos field on the My events page
	And User sees '0' in Pending for review field on the My events page
	And User sees '0' in Filtered photos field on the My events page
	And User sees 'Last update: 16 March 2018' in Manage photos section on the My events page
	And User sees that '2' photos are displayed on the My events page