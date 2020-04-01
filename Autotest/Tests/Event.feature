Feature: Event
	Check that all features on the Event page works fine

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in
	When User clicks on the 'Events' button on the header
	And User clicks on the 'All events' item in Events menu
	Then User sees 'CrowdPic Events:' page title

@Regression
Scenario: Check that user can open photo on the Event page
	When User clicks on the 'test32' on the All events
	Then User sees 'test32' event title on the Event page
	When User clicks '1' photo on Event page
	Then User sees 'https://pictureprojectstorage.blob.core.windows.net/photos-preview/45531f28-fe17-402f-bd01-8ee2dab9f685_preview.jpg' photo on View photo page
	And User sees 'Pasha' photographer's name in view photo menu 
	And User sees '25 March 2020' date in view photo menu

@Regression
Scenario: Check transition to the Find your photos
	When User clicks on the 'test32' on the All events
	Then User sees 'test32' event title on the Event page
	When User clicks on the FIND MY PHOTOS button on the Event page
	Then User sees 'Find your photos' page title
	And User sees 'test32' event name on the Find your photos page

@Regression
Scenario: Check transition to the Upload photos
	When User clicks on the 'test32' on the All events
	Then User sees 'test32' event title on the Event page
	When User clicks on the ADD PHOTOS button on the Event page
	Then User sees 'Upload photos' page title
	And User sees 'test32' event name on the Upload photos page
	