<?php
require('db_connection.php');
$fk_subject_id = $_POST['fk_subject_id'];

$sql = "SELECT * FROM modules_tbl WHERE fk_subject_id = $fk_subject_id";
$result = $conn->query($sql);

$modules = array();
if ($result->num_rows > 0) {
    while ($row = $result->fetch_assoc()) {
        $modules[] = $row;
    }
}

echo json_encode($modules);
$conn->close();
?>
