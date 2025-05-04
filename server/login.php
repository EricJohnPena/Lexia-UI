<?php
require ('db_connection.php');

$loginUser = $_POST['loginUser'];
$loginPass = $_POST['loginPass'];

$sql = "SELECT student_id, fk_section_id, first_name, last_name 
        FROM students_tbl 
        WHERE username = ? 
        AND password = ?";

$stmt = $conn->prepare($sql);
$stmt->bind_param("ss", $loginUser, $loginPass);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows > 0) {
    $row = $result->fetch_assoc();
    echo json_encode($row);
} else {
    echo "Incorrect Username or Password. Try again.";
}
$conn->close();
