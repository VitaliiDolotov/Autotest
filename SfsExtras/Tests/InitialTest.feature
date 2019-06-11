Feature: InitialTest
	Test search functionality on google.com

@Regression
Scenario: Check that search returns results
	Given I have opened google website
	And I have entered 'New York' text in the search field
	When I have clicked Search button
	Then results found contains 'New York' text

@Regression
Scenario: Check that search returns results - Negative results
	Given I have opened google website
	And I have entered 'California' text in the search field
	When I have clicked Search button
	Then results found contains 'some_textTESTAAA' text
