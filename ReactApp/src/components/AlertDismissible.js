import { useState } from 'react';
import Alert from 'react-bootstrap/Alert';
import Button from 'react-bootstrap/Button';

function AlertDismissible({alertMessage}) {
  const [show, setShow] = useState(true);

  return (
    alertMessage && <>
      <Alert show={show} variant="success">
        <Alert.Heading className="d-flex justify-content-center">{alertMessage}</Alert.Heading>
      </Alert>
    </>
  );
}

export default AlertDismissible;