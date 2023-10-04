import Alert from 'react-bootstrap/Alert';

function AlertDismissible({alertMessage}) {
  return (
    alertMessage && <>
      <Alert show={true} variant="success">
        <Alert.Heading className="d-flex justify-content-center">{alertMessage}</Alert.Heading>
      </Alert>
    </>
  );
}

export default AlertDismissible;