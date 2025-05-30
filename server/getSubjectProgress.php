<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

require_once 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $subject_id = $_POST['subject_id'] ?? null;
    $student_id = $_POST['student_id'] ?? null;

    if (!$subject_id || !$student_id) {
        echo json_encode(['error' => 'Missing required parameters']);
        exit;
    }

    try {
        $stmt = $conn->prepare("
            SELECT m.module_number, 
                   COUNT(sp.id) as completed_count
            FROM modules_tbl m
            LEFT JOIN students_progress_tbl sp ON m.module_number = sp.module_number 
                AND sp.fk_subject_id = m.fk_subject_id 
                AND sp.student_id = ?
            WHERE m.fk_subject_id = ?
            GROUP BY m.module_number
            ORDER BY m.module_number
        ");
        
        $stmt->bind_param("ii", $student_id, $subject_id);
        $stmt->execute();
        $result = $stmt->get_result();
        
        $modules = [];
        while ($row = $result->fetch_assoc()) {
            $modules[] = [
                'module_number' => $row['module_number'],
                'completed_count' => (int)$row['completed_count']
            ];
        }
        
        if (empty($modules)) {
            echo json_encode(['error' => 'No modules found for this subject']);
        } else {
            echo json_encode($modules);
        }
    } catch (Exception $e) {
        echo json_encode(['error' => $e->getMessage()]);
    }
} else {
    echo json_encode(['error' => 'Invalid request method']);
}
?> 