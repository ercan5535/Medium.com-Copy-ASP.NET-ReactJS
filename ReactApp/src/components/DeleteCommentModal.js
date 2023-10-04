import Button from 'react-bootstrap/Button';
import React, {useState, useEffect} from 'react'
import Modal from 'react-bootstrap/Modal';

export default function DeleteCommentModal({isOpen, onClose, handleDelete}) {
    return (
        <>
          <Modal show={isOpen} onHide={onClose} className='delete-comment-modal'>
            <Modal.Header closeButton>
              <Modal.Title>Delete Comment{isOpen}</Modal.Title>
            </Modal.Header>
            <Modal.Body>Are you sure you want to continue with your action?</Modal.Body>
            <Modal.Footer>
              <Button variant="outline-secondary" onClick={onClose}>
                Close
              </Button>
              <Button variant="outline-danger" onClick={handleDelete}>
                Delete
              </Button>
            </Modal.Footer>
          </Modal>
        </>
      );
    }