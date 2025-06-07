<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

// Enable error logging
error_reporting(E_ALL);
ini_set('display_errors', 1);

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
            $profile_picture_id = $row['fk_profile_picture_id'];
            
            // Log the raw value from database
            error_log("Raw profile picture ID from database: " . var_export($profile_picture_id, true));
            
            // Convert to string and ensure it's not empty
            $profile_picture_id = $profile_picture_id !== null ? (string)$profile_picture_id : '';
            
            // Log the processed value
            error_log("Processed profile picture ID: " . var_export($profile_picture_id, true));
            
            $response = [
                'success' => true,
                'has_profile_picture' => !empty($profile_picture_id),
                'profile_picture_id' => $profile_picture_id
            ];
            
            // Log the final response
            error_log("Sending response: " . json_encode($response));
            
            echo json_encode($response);
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