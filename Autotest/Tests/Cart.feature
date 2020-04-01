Feature: Cart
	Check that all features of the cart work fine

Background: 
	Given User is on Crowdpic Homepage
	When User click Sign In button on Homepage
	Then Login Page is displayed to the user
	When User provides the Login and Password and clicks on the login button
	Then User has successfully logged in

@Regression
Scenario: Check empty cart
	When User presses cart icon
	Then User sees 'Your cart is empty' on Cart page
	Then User sees 'Shopping cart' cart page title
