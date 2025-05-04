<?php
require ('db_connection.php');

$sql = "SELECT fk_section_id FROM students_tbl WHERE student_id = $studentId";
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