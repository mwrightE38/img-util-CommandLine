<?php

/*
 * Description: Sample web page to test Event38 API calls.
 * Environment: Amazon AWS
 * Date: 10/16/14
 * 
 * Requirements: 
 * 	Drupal 7.x
 * 	Services 3.x
 * 	Services Views 
 * 	OAuth
 * 
 * Reference article used to create this test page: 
 * 	https://www.drupal.org/node/1354202
 */


// Variables containing data for the NODE and FILE to create.

	$type = 'test_app'; // node type
	$title = 'Testing Testing - 1 - app.event38.com'; // title 
	$body = '<p>Body lorem ipsum</p>'; // body
	$first_name = 'booo'; // first name
	$last_name = 'yahhhhh'; // last name
	$filename = '/Users/laxg/Downloads/azurelogo.jpg';  // file to upload and attach with content
	$services_url = 'http://ec2-54-244-149-251.us-west-2.compute.amazonaws.com'; // RESTful services end-point



/*
 * Step 1: 
 * 	Server REST - user.login
 * 	Login as an user and get the token from the session to post NODE and FILE information.
 */

	// REST Server URL for auth
	$request_url = $services_url . '/api/products/user/login';
	// User data
	$user_data = array(
	  'username' => 'apiuser',
	  'password' => 'apiuser',
	);
	$user_data = http_build_query($user_data);
	// cURL
	$curl = curl_init($request_url);
	curl_setopt($curl, CURLOPT_HTTPHEADER, array('Accept: application/json')); // Accept JSON response
	curl_setopt($curl, CURLOPT_POST, 1); // Do a regular HTTP POST
	curl_setopt($curl, CURLOPT_POSTFIELDS, $user_data); // Set POST data
	curl_setopt($curl, CURLOPT_HEADER, FALSE);  // Ask to not return Header
	curl_setopt($curl, CURLOPT_RETURNTRANSFER, TRUE);
	curl_setopt($curl, CURLOPT_FAILONERROR, TRUE);
	$response = curl_exec($curl);
	$http_code = curl_getinfo($curl, CURLINFO_HTTP_CODE);
	// Check if login was successful
	if ($http_code == 200) {
	  // Convert json response as array
	  $logged_user = json_decode($response);
	  print("Logged in successfully." . "\n<br>" . "Session Name: " . $logged_user->session_name . "\n<br>" );
	}
	else {
	  // Get error msg
	  $http_message = curl_error($curl);
	  die('Auth error ' . $http_message);
	}
	
	// Define cookie session
	$cookie_session = $logged_user->session_name . '=' . $logged_user->sessid;
	// Store the token for subsequent form POSTs
	$token = $logged_user->token;

/*
 * Step 2: 
 * 	Server REST - file.create
 * 	Upload the file / image that is to be associated with the node.  
 * 	Successful post of the file will return a File ID (fid) and a path to the file (URI).
 */
	// basic file validation	
	if(!file_exists($filename)) {
	  die('File not exists');
	}
	if(!is_readable($filename)) {
	  die('File not readable');
	}
	
	// create the json FILE data
	$file = array(
	  'filesize' => filesize($filename),
	  'filename' => basename($filename),
	  'file' => base64_encode(file_get_contents($filename)),
	  'uid' => $logged_user->user->uid,
	);
	
	$file = http_build_query($file);
	
	// REST Server URL for file upload
	$request_url = $services_url . '/api/products/file';
	
	// cURL - use cURL to post the file to DRUPAL 
	
	$curl = curl_init($request_url);
	curl_setopt($curl, CURLOPT_HTTPHEADER, array('Content-type: application/x-www-form-urlencoded','Accept: application/json'));
	curl_setopt($curl, CURLOPT_HTTPHEADER, array('X-CSRF-Token: ' . $token));
	curl_setopt($curl, CURLOPT_POST, 1); // Do a regular HTTP POST
	curl_setopt($curl, CURLOPT_POSTFIELDS, $file); // Set POST data
	curl_setopt($curl, CURLOPT_HEADER, FALSE);  // Ask to not return Header
	curl_setopt($curl, CURLOPT_COOKIE, "$cookie_session"); // use the previously saved session
	curl_setopt($curl, CURLOPT_RETURNTRANSFER, TRUE);
	curl_setopt($curl, CURLOPT_FAILONERROR, TRUE);
	$response = curl_exec($curl);
	$http_code = curl_getinfo($curl, CURLINFO_HTTP_CODE);
	
	// Check if the file post was successful
	if ($http_code == 200) {
	  // Convert json response as array
	  $file_data = json_decode($response);
	 // convert xml response into an array
	  // read the XML database of aminoacids
	  $data = $response;
	  $file_data = new SimpleXMLElement($response);
	  
	  /* Search for <a><b><c> */
	  
	  $dom = new DOMDocument;
	  $dom->loadXML($response);
	  $xpath = new DOMXpath($dom);
	  
	  $dicts = $xpath->query("/result/fid");
	
	  foreach($dicts as $dict) {
	  	if ($dict->nodeName == 'fid') {
	  		$fid = $dict->nodeValue;
	  	}
	  	continue;
	  }
	  
	  $dicts = $xpath->query("/result/uri");
	  
	  foreach($dicts as $dict) {
	  	if ($dict->nodeName == 'uri') {
	  		$uri = $dict->nodeValue;
	  	}
	  	continue;
	  }
	  
	}
	else {
	  // Get error msg
	  $http_message = curl_error($curl);
	  die('Sending file error<br>' . $http_message . "\n<br>" . "Response: " . $response . "\n<br>" . "File: " . $file);
	}
	// file id (nessesary for node)
	
	print("File id:" . $fid . "\n<br>" . "URI: " . $uri . "\n<br>");

/*
 * Step 3: 
 * 	Server REST - node.create
 * 	Create the node and associate the file id to it and post the node using JSON.
 */
	// REST Server URL (default from services module)
	$request_url = $services_url . '/api/products/node';
	
	// Node data - prepare data in JSON format
	
	$node_data = array(
	  'title' => $title,
	  'type' => $type,
	  'body[und][0]' => array ('value' => $body ),
	  //'field_first_name[und][0]' => array('value'=>$first_name),
	  //'field_last_name[und][0]' => array('value'=>$first_name),	
	  //'taxonomy[tags][' . $vid . ']' => $tags,
	  'field_photo[und][0]' => array('fid' => $fid, 'uid' => $logged_user->user->uid, 'list' => 1, 'data'=>NULL)
	);
	
	$node_data = http_build_query($node_data);
	// cURL
	$curl = curl_init($request_url);
	curl_setopt($curl, CURLOPT_HTTPHEADER, array('Accept: application/json')); // Accept JSON response
	curl_setopt($curl, CURLOPT_HTTPHEADER, array('X-CSRF-Token: ' . $token));
	curl_setopt($curl, CURLOPT_POST, 1); // Do a regular HTTP POST
	curl_setopt($curl, CURLOPT_POSTFIELDS, $node_data); // Set POST data
	curl_setopt($curl, CURLOPT_HEADER, FALSE);  // Ask to not return Header
	curl_setopt($curl, CURLOPT_COOKIE, "$cookie_session"); // use the previously saved session
	curl_setopt($curl, CURLOPT_RETURNTRANSFER, TRUE);
	curl_setopt($curl, CURLOPT_FAILONERROR, TRUE);
	$response = curl_exec($curl);
	$http_code = curl_getinfo($curl, CURLINFO_HTTP_CODE);
	
	// Check if node post was successful
	if ($http_code == 200) {
	  // Convert json response as array
	  $node = json_decode($response);
	  print("Node Entry Successful: " . $node);
	}
	else {
	  // Get error msg
	  $http_message = curl_error($curl);
	  die('Getting data error<br>' . $http_message . "\n<br>");
	}
	print_r($node);
	
	// Validate in Drual by logging in and ensuring that the data was created.
	
?>