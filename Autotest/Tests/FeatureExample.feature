Feature: FeatureExample
	Simple calculator for adding two numbers

Background: Pre-Conditions
	Given User is logged in to application

@mytag
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120