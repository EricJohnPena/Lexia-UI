<?php
// Connect to MySQL database
require ('db_connection.php');

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Retrieve data from student_progress_tbl
$result = $conn->query("SELECT * FROM students_progress_tbl");

// Create an array to store the data
$data = array();

while ($row = $result->fetch_assoc()) {
    $data[] = $row;
}

// Close the connection
$conn->close();

// Output the data as JSON
echo json_encode($data);
?>