<?php
require ('db_connection.php');

//$sectionId = $_POST['fk_section_id'];
$sectionId = 1;

$sql = "SELECT username FROM students_tbl WHERE fk_section_id = $sectionId";
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