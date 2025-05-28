<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $student_id = isset($_GET['student_id']) ? $_GET['student_id'] : null;

    if ($student_id === null) {
        echo json_encode([
            'success' => false,
            'error' => 'Student ID is required'
        ]);
        exit;
    }

    try {
        // Check if the student has a profile picture set
        $query = "SELECT fk_profile_picture_id FROM students_tbl WHERE student_id = ?";
        $stmt = $conn->prepare($query);
        $stmt->bind_param("s", $student_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows > 0) {
            $row = $result->fetch_assoc();
            echo json_encode([
                'success' => true,
                'has_profile_picture' => $row['fk_profile_picture_id'] !== null,
                'profile_picture_id' => $row['fk_profile_picture_id']
            ]);
        } else {
            echo json_encode([
                'success' => false,
                'error' => 'Student not found'
            ]);
        }
    } catch (Exception $e) {
        echo json_encode([
            'success' => false,
            'error' => $e->getMessage()
        ]);
    }
} else {
    echo json_encode([
        'success' => false,
        'error' => 'Invalid request method'
    ]);
}
?> 