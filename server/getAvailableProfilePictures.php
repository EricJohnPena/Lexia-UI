<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET');
header('Access-Control-Allow-Headers: Content-Type');

include 'db_connection.php';

try {
    // Get all profile pictures
    $query = "SELECT picture_id, image_path FROM profile_pictures ORDER BY picture_id";
    $result = $conn->query($query);

    if ($result) {
        $pictures = array();
        while ($row = $result->fetch_assoc()) {
            $pictures[] = array(
                'picture_id' => (int)$row['picture_id'],
                'image_path' => $row['image_path']
            );
        }

        echo json_encode([
            'success' => true,
            'pictures' => $pictures
        ]);
    } else {
        echo json_encode([
            'success' => false,
            'error' => 'Failed to fetch profile pictures'
        ]);
    }
} catch (Exception $e) {
    echo json_encode([
        'success' => false,
        'error' => $e->getMessage()
    ]);
}
?> 