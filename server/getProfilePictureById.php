<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $picture_id = isset($_GET['picture_id']) ? $_GET['picture_id'] : null;

    if ($picture_id === null) {
        echo json_encode(['error' => 'Picture ID is required']);
        exit;
    }

    try {
        // Get the profile picture path
        $query = "SELECT image_path FROM profile_pictures WHERE picture_id = ?";
        $stmt = $conn->prepare($query);
        $stmt->bind_param("i", $picture_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows > 0) {
            $row = $result->fetch_assoc();
            echo json_encode([
                'success' => true,
                'image_path' => $row['image_path']
            ]);
        } else {
            echo json_encode([
                'success' => false,
                'error' => 'Profile picture not found'
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