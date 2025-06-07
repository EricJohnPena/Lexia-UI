<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $student_id = isset($_POST['student_id']) ? $_POST['student_id'] : null;
    $old_password = isset($_POST['old_password']) ? $_POST['old_password'] : null;
    $new_password = isset($_POST['new_password']) ? $_POST['new_password'] : null;

    if ($student_id === null || $old_password === null || $new_password === null) {
        echo json_encode([
            'success' => false,
            'error' => 'Missing required fields'
        ]);
        exit;
    }

    try {
        // First verify the old password
        $query = "SELECT loginPass FROM students_tbl WHERE student_id = ?";
        $stmt = $conn->prepare($query);
        $stmt->bind_param("i", $student_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows === 0) {
            echo json_encode([
                'success' => false,
                'error' => 'User not found'
            ]);
            exit;
        }

        $row = $result->fetch_assoc();
        if (!password_verify($old_password, $row['loginPass'])) {
            echo json_encode([
                'success' => false,
                'error' => 'Current password is incorrect'
            ]);
            exit;
        }

        // Hash the new password
        $hashed_password = password_hash($new_password, PASSWORD_DEFAULT);

        // Update the password
        $update_query = "UPDATE students_tbl SET loginPass = ? WHERE student_id = ?";
        $update_stmt = $conn->prepare($update_query);
        $update_stmt->bind_param("si", $hashed_password, $student_id);
        
        if ($update_stmt->execute()) {
            echo json_encode([
                'success' => true
            ]);
        } else {
            echo json_encode([
                'success' => false,
                'error' => 'Failed to update password'
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