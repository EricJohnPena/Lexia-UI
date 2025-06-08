<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

require_once 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Get the POST data
    $data = json_decode(file_get_contents('php://input'), true);
    
    // If no JSON data, try regular POST
    if ($data === null) {
        $data = $_POST;
    }

    // Check if student_id and new_password are provided
    if (!isset($data['student_id']) || !isset($data['new_password'])) {
        echo json_encode([
            'success' => false,
            'error' => 'Student ID and new password are required'
        ]);
        exit;
    }

    $student_id = $data['student_id'];
    $new_password = $data['new_password'];
    $old_password = isset($data['old_password']) ? $data['old_password'] : null;

    try {
        // Start transaction
        $conn->begin_transaction();

        // If old_password is provided, verify it first
        if ($old_password !== null) {
            $stmt = $conn->prepare("SELECT password FROM students_tbl WHERE student_id = ?");
            $stmt->bind_param("s", $student_id);
            $stmt->execute();
            $result = $stmt->get_result();
            
            if ($result->num_rows === 0) {
                throw new Exception("Student not found");
            }

            $row = $result->fetch_assoc();
            if (!password_verify($old_password, $row['password'])) {
                throw new Exception("Current password is incorrect");
            }
        }

        // Hash the new password
        $hashed_password = password_hash($new_password, PASSWORD_DEFAULT);

        // Update the password
        $stmt = $conn->prepare("UPDATE students_tbl SET password = ? WHERE student_id = ?");
        $stmt->bind_param("ss", $hashed_password, $student_id);
        
        if (!$stmt->execute()) {
            throw new Exception("Failed to update password");
        }

        // If this is a first-time password change (no old_password provided)
        if ($old_password === null) {
            // Update is_first_time to false
            $stmt = $conn->prepare("UPDATE students_tbl SET is_first_time = 0 WHERE student_id = ?");
            $stmt->bind_param("s", $student_id);
            
            if (!$stmt->execute()) {
                throw new Exception("Failed to update first-time status");
            }
        }

        // Commit transaction
        $conn->commit();

        echo json_encode([
            'success' => true,
            'message' => 'Password updated successfully'
        ]);

    } catch (Exception $e) {
        // Rollback transaction on error
        $conn->rollback();
        
        echo json_encode([
            'success' => false,
            'error' => $e->getMessage()
        ]);
    }

    $stmt->close();
} else {
    echo json_encode([
        'success' => false,
        'error' => 'Invalid request method'
    ]);
}

$conn->close();
?> 