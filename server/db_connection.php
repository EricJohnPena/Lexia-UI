<?php
$servername = "localhost";
$username = "u255088217_pena";
$password = "Lexiadb123";
$dbname = "u255088217_mydb";



// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}
?>