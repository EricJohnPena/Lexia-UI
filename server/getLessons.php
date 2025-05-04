<?php
require ('db_connection.php');

$subject_id = $_POST['fk_subject_id'];
$module_number = $_POST['module_number'];
$sql = "SELECT l.*
FROM lessons_tbl l
JOIN modules_tbl m 
  ON l.fk_module_number = m.module_number
 AND l.fk_subject_id = m.fk_subject_id
WHERE l.fk_subject_id = ? 
  AND l.fk_module_number = ?";
$stmt = $conn->prepare($sql);
$stmt->bind_param("ii", $subject_id, $module_number);
$stmt->execute();
$result = $stmt->get_result();

$lessons = [];
while ($row = $result->fetch_assoc()) {
    $lessons[] = $row;
}

echo json_encode($lessons);
$stmt->close();
$conn->close();
?>
