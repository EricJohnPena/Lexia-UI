<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $student_id = isset($_GET['student_id']) ? $_GET['student_id'] : null;

    if ($student_id === null) {
        echo json_encode(['error' => 'Student ID is required']);
        exit;
    }

    try {
        // Get the profile picture path for the student
        $query = "SELECT pp.image_path 
                 FROM profile_pictures pp 
                 INNER JOIN students_tbl st ON st.fk_profile_picture_id = pp.picture_id 
                 WHERE st.student_id = ?";
        
        $stmt = $conn->prepare($query);
        $stmt->bind_param("i", $student_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows > 0) {
            $row = $result->fetch_assoc();
            echo json_encode([
                'success' => true,
                'image_path' => $row['image_path']
            ]);
        } else {
            // Return default profile picture if no custom picture is set
            echo json_encode([
                'success' => true,
                'image_path' => 'default_profile.png'
            ]);
        }
    } catch (Exception $e) {
        echo json_encode(['error' => $e->getMessage()]);
    }
} else {
    echo json_encode(['error' => 'Invalid request method']);
}
?> 