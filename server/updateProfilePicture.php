<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $student_id = isset($_POST['student_id']) ? $_POST['student_id'] : null;
    $picture_id = isset($_POST['picture_id']) ? $_POST['picture_id'] : null;

    if ($student_id === null || $picture_id === null) {
        echo json_encode(['error' => 'Student ID and Picture ID are required']);
        exit;
    }

    try {
        // Update the student's profile picture
        $query = "UPDATE students_tbl SET fk_profile_picture_id = ? WHERE student_id = ?";
        $stmt = $conn->prepare($query);
        $stmt->bind_param("ii", $picture_id, $student_id);
        
        if ($stmt->execute()) {
            echo json_encode([
                'success' => true
            ]);
        } else {
            echo json_encode([
                'success' => false,
                'error' => 'Failed to update profile picture'
            ]);
        }
    } catch (Exception $e) {
        echo json_encode([
            'success' => false,
            'error' => $e->getMessage()
        ]);
    }
} else {
    echo json_encode(['error' => 'Invalid request method']);
}
?> 