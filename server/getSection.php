<?php
require ('db_connection.php');

$section_id =  $_POST["section_id"];

$sql = "SELECT section_name FROM sections_tbl WHERE section_id = $section_id";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
  // output data of each row
  while($row = $result->fetch_assoc()) {
    $rows[] = $row;
  }
  echo json_encode($rows);
} else {
  echo "0";
}
$conn->close();
?> 