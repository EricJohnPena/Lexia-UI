<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "mydb";

$loginUser = $_POST['loginUser'];
$loginPass = $_POST['loginPass'];
$first_name = $_POST['first_name'];
$last_name = $_POST['last_name'];
$email = $_POST['email'];

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT username FROM students_tbl where username = '". $loginUser ."'";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
  echo "Username already exists";
}else{
    $sql = "INSERT INTO students_tbl (username, password, first_name, last_name, email, section_id) 
    VALUES ('$loginUser', '$loginPass','$first_name', '$last_name','$email', '2')";

if ($conn->query($sql) === TRUE) {
    echo "New record created successfully";
  } else {
    echo "Error: " . $sql . "<br>" . $conn->error;
  }
}   
$conn->close();
?> 