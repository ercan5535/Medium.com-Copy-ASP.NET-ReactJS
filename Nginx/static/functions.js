async function login_status_request(){
  // first check access token
  const access_response = await fetch(
      'http://localhost:80/auth/api/authentication/check/', {
      method: 'GET',
      credentials: 'include',
    });
  if (access_response.ok){
    console.log('Access token is valid');
    const user_data = await access_response.json()
    return user_data
  }

  // second check refresh token
  const refresh_response = await fetch(
    'http://localhost:80/auth/api/authentication/refresh/', {
    method: 'GET',
    credentials: 'include',
  });
  if (refresh_response.ok){
    console.log('Refresh token is valid');
    const user_data = await refresh_response.json()
    return user_data
  };
  
  // Display login page
  console.log("Both token are invalid")
  return false
}

async function login_request(login_data){
  // first check access token
  const login_response = await fetch(
    "http://localhost:80/auth/api/authentication/login/", {
    method: "POST",
    credentials: 'include',
    body: login_data,
    headers: {
      "Content-Type": "application/json"
    }});
  if (login_response.ok){
    const user_data = await login_response.json()
    return user_data
  }
  else{
    document.getElementById("unlogged_in_error_message").textContent = "Invalid credentials"; // Update error message
    document.getElementById("unlogged_in_error_message").style.display = "block"; // Show error message
    return false
  }
}

async function register_request(request_data){
  const register_response = await fetch(
    "http://localhost:80/auth/api/authentication/register/", {
    method: "POST",
    credentials: 'include',
    body: request_data,
    headers: {
      "Content-Type": "application/json"
    }});
  if (register_response.ok){
    // Display index page
    document.querySelector('#register_form').style.display = 'none';
    document.querySelector('#login_form').style.display = 'block';
    document.getElementById("unlogged_in_alert_message").textContent = "Registered succesfully"; // Update error message
    document.getElementById("unlogged_in_alert_message").style.display = "block";
    const response_data = await register_response.json()
    console.log(response_data)
    return true
  }
  else{
    const response_data = await register_response.text();
    document.getElementById("unlogged_in_error_message").textContent = response_data; // Update error message
    document.getElementById("unlogged_in_error_message").style.display = "block"; // Show error message
    return false
  }
}

async function new_transaction_request(transaction_data){
  const new_transaction_response = await fetch(
    "http://localhost:80/transactions/api/transactions/transactions/", {
    method: "POST",
    credentials: 'include',
    body: transaction_data,
    headers: {
      "Content-Type": "application/json"
    }});
  if (new_transaction_response.ok){
    // Create alert
    document.getElementById("logged_in_alert_message").textContent = "New Transaction Added Successfully!"; // Update error message
    document.getElementById("logged_in_alert_message").style.display = "block";
    const transaction_data = await new_transaction_response.json()
    console.log(transaction_data)
    return transaction_data
  }
  else if (new_transaction_response.status == 401){
    manage_page_by_login_status()
  }
  else{
    const transaction_data = await new_transaction_response.text();
    document.getElementById("logged_in_error_message").textContent = transaction_data; // Update error message
    document.getElementById("logged_in_error_message").style.display = "block"; // Show error message
    return false
  }
}

async function get_transactions_request(){
  const get_transactions_response = await fetch(
    "http://localhost:80/transactions/api/transactions/transactions/", {
    method: "GET",
    credentials: 'include',
  });
  if (get_transactions_response.ok){
    // Create alert
    const transaction_data = await get_transactions_response.json()
    return transaction_data
  }
  else if (get_transactions_response.status == 401){
    manage_page_by_login_status()
  }
  return false
}

async function update_transaction_request(transaction_id, transaction_data){
  const delete_transaction_response = await fetch(
    `http://localhost:80/transactions/api/transactions/detail/${transaction_id}`, {
      method: "PATCH",
      credentials: 'include',
      body: transaction_data,
      headers: {
        "Content-Type": "application/json"
      }
    });
  if (delete_transaction_response.ok){
    // Create alert
    document.getElementById("logged_in_alert_message").textContent = "Transaction Updated Successfully!"; // Update error message
    document.getElementById("logged_in_alert_message").style.display = "block";
    load_transactions_table_page()
  }
  else if (delete_transaction_response.status == 401){
    manage_page_by_login_status()
  }
  else{
    const transaction_data = await delete_transaction_response.text();
    document.getElementById("logged_in_error_message").textContent = transaction_data; // Update error message
    document.getElementById("logged_in_error_message").style.display = "block"; // Show error message
  }
}

async function delete_transaction_request(transaction_id){
  const delete_transaction_response = await fetch(
    `http://localhost:80/transactions/api/transactions/detail/${transaction_id}`, {
    method: "DELETE",
    credentials: 'include',
  });
  if (delete_transaction_response.ok){
    // Create alert
    document.getElementById("logged_in_alert_message").textContent = "Transaction Deleted Successfully!"; // Update error message
    document.getElementById("logged_in_alert_message").style.display = "block";
    load_transactions_table_page()
  }
  else if (delete_transaction_response.status == 401){
    manage_page_by_login_status()
  }
}

async function toggle_confirm_transaction_request(transaction_id){
  const confirm_transaction_response = await fetch(
    `http://localhost:80/transactions/api/transactions/detail/${transaction_id}`, {
    method: "HEAD",
    credentials: 'include',
  });
  if (confirm_transaction_response.ok){
    return true
  }
  else if (confirm_transaction_response.status == 401){
    manage_page_by_login_status()
  }
  return false
}

async function logout_request(){
  const logout_response = await fetch(
    "http://localhost:80/auth/api/authentication/logout/", {
    method: "GET",
    credentials: 'include',
  });
  if (logout_response.ok){
      // Reload page
      window.location.reload();
      // Delete user data from local storage
      localStorage.removeItem("userData")
  }
}

// Check login status and display logged or not logged div
function manage_page_by_login_status(){
  login_status_request().then(userData => {
    // if current status logged in
    if (userData){
      // Store user data on local storage
      localStorage.setItem("userData", JSON.stringify(userData))
      // Display logged in page
      document.querySelector('#not_logged_in_page').style.display = 'none';
      document.querySelector('#logged_in_page').style.display = 'block';
      // initially show transactions tables
      load_transactions_table_page()
    }
    // if current status is not logged in
    else{
      // Clear user data from local storage
      localStorage.removeItem("userData")
      // Display not logged in page(login, register)
      document.querySelector('#not_logged_in_page').style.display = 'block';
      document.querySelector('#logged_in_page').style.display = 'none';
      // Initially show login form
      document.querySelector('#register_form').style.display = 'none';
      document.querySelector('#login_form').style.display = 'block';
    }
  }) 
}

function create_transactions_table(transaction_data){
  var tableBody = document.querySelector("#transactions_table tbody");

  // Clear existing table rows
  tableBody.innerHTML = "";

  // Generate new table rows
  transaction_data.forEach(function(transaction) {
    var row = document.createElement("tr");
    // Add transaction id button to row
    var idCell = document.createElement("td");
    var idButton = document.createElement("button");
    idButton.textContent = transaction.id;

      // Add onclick event to idCell
    idButton.onclick = function() {
      load_edit_transaction_page(
        transaction.id,
        transaction.department,
        transaction.amount,
        transaction.created_by,
        transaction.is_confirmed,
        transaction.created_at);
    };
    idCell.appendChild(idButton);
    row.appendChild(idCell);
    // add transaction department to row
    var departmentCell = document.createElement("td");
    departmentCell.textContent = transaction.department;
    row.appendChild(departmentCell);
    // add transaction amount to row
    var amountCell = document.createElement("td");
    amountCell.textContent = transaction.amount;
    row.appendChild(amountCell);
    // add transaction created at to row
    var createdbyCell = document.createElement("td");
    createdbyCell.textContent = transaction.created_by;
    row.appendChild(createdbyCell);
    // add transaction is confirmed to row
    var confirmStatusCell = document.createElement("td");
    if (transaction.is_confirmed){
      confirmStatusCell.textContent = "Confirmed";
      confirmStatusCell.style.color = "green";
    }
    else{
      confirmStatusCell.textContent = "Not Confirmed";
      confirmStatusCell.style.color = "red";
    }
    row.appendChild(confirmStatusCell);
    // add transaction created atto row
    var createdAtCell = document.createElement("td");
    createdAtCell.textContent = transaction.created_at;
    row.appendChild(createdAtCell);

    tableBody.appendChild(row);
  })
}

function load_transactions_table_page(){
  // Hide other pages
  document.getElementById("logged_in_error_message").style.display = "none";
  document.querySelector('#add_transaction_form').style.display = 'none';
  document.querySelector('#edit_transaction_div').style.display = 'none';

  get_transactions_request().then(data => {
    transactions_data = data
    console.log(transactions_data)
    // Fill the table with transactions data
    if (transactions_data){
      // Create table element
      create_transactions_table(transactions_data)
      // Show transactions table page
      document.querySelector('#all_transactions_div').style.display = 'block';
    }
  });
}

function load_add_transaction_page(){
  document.getElementById("logged_in_error_message").style.display = "none";
  document.querySelector('#edit_transaction_div').style.display = 'none';
  document.querySelector('#all_transactions_div').style.display = 'none';
  document.querySelector('#add_transaction_form').style.display = 'block';
}

function load_edit_transaction_page(transaction_id, department, amount, created_by, is_confirmed, created_at){
  document.getElementById("logged_in_error_message").style.display = "none";
  document.querySelector('#edit_transaction_div').style.display = 'block';
  document.querySelector('#all_transactions_div').style.display = 'none';
  document.querySelector('#add_transaction_form').style.display = 'none';
  // only for manager, default dont display
  document.getElementById("confirm_transaction_button").style.display = "none";
  
  // Set department
  document.getElementById('TransactionDetailDepartment').value = department
  // Set amount
  document.getElementById('TransactionDetailAmount').value = amount
  // Set confirmed status
  if (is_confirmed){
    document.getElementById('TransactionDetailConfirmed').value = "Confirmed"
    document.getElementById('TransactionDetailConfirmed').style.color = "green"; 
  }
  else{
    document.getElementById('TransactionDetailConfirmed').value = "Not Confirmed"
    document.getElementById('TransactionDetailConfirmed').style.color = "red"; 
  }
  // Set created by
  document.getElementById('TransactionDetailCreatedBy').value = created_by
  // Set created at
  document.getElementById('TransactionDetailCreatedAt').value = created_at

  // Assign form submission to update function with associate transaction id
  // Refresh event listeners on form
  updateTransactionForm = document.getElementById('update_transaction_form');
  updateTransactionForm.replaceWith(updateTransactionForm.cloneNode(true));
  // Then add a new event listener
  refreshedUpdateForm = document.getElementById('update_transaction_form');
  refreshedUpdateForm.addEventListener("submit", function(event) {
    // Prevent default form submission
    event.preventDefault(); 

    // Get the values entered by the user
    var department = document.getElementById("TransactionDetailDepartment").value;
    var amount = document.getElementById("TransactionDetailAmount").value;
  
    // Create a data object with the values
    var data = {
      "department": department,
      "amount": amount
    };
  
    console.log(data)
    // Convert the data object to a JSON string
    var jsonData = JSON.stringify(data);
  
    update_transaction_request(transaction_id, jsonData)
  });

  // Assign Delete button to function with associate transaction id
  // Refresh event listeners on button
  deleteButtonElement = document.getElementById('delete_transaction_button');
  deleteButtonElement.replaceWith(deleteButtonElement.cloneNode(true));
  // Then add a new event listener
  refreshedDeleteButton = document.getElementById('delete_transaction_button');
  refreshedDeleteButton.addEventListener('click', () => delete_transaction_request(transaction_id));


  // Confirm buttons(only for manager)
  // Get USER_DATA from localstorage
  USER_DATA = JSON.parse(localStorage.getItem("userData"))
  if(USER_DATA.is_manager){
    // Define buttons details
    confirmationButton = document.getElementById("confirm_transaction_button");
    if (is_confirmed){
      confirmationButton.className = "btn btn-danger";
      confirmationButton.textContent = "Disconfirm Transaction";
    }
    else{
      confirmationButton.className = "btn btn-success";
      confirmationButton.textContent = "Confirm Transaction";
    }
    // display button
    confirmationButton.style.display = "block"
    // Refresh event listeners on button
    confirmationButton.replaceWith(confirmationButton.cloneNode(true));
    refreshedConfirmationButton = document.getElementById('confirm_transaction_button');
    // Add click event listener to toggle confirm status
    refreshedConfirmationButton.addEventListener('click', function(){
      if(is_confirmed){
        if (toggle_confirm_transaction_request(transaction_id)){
          // Update button
          refreshedConfirmationButton.className = "btn btn-success";
          refreshedConfirmationButton.textContent = "Confirm Transaction";

          //Update detail view
          document.getElementById('TransactionDetailConfirmed').value = "Not Confirmed";
          document.getElementById('TransactionDetailConfirmed').style.color = "red"; 

          // Toggle confirm status
          is_confirmed = !is_confirmed
        }
      }
      else{
        if (toggle_confirm_transaction_request(transaction_id)){
          // Update button
          refreshedConfirmationButton.className = "btn btn-danger";
          refreshedConfirmationButton.textContent = "Disconfirm Transaction";

          //Update detail view
          document.getElementById('TransactionDetailConfirmed').value = "Confirmed";
          document.getElementById('TransactionDetailConfirmed').style.color = "green"; 

          // Toggle confirm status
          is_confirmed = !is_confirmed
        };
      }
    })
  }
}
// Load initial page(logged_in or not_logged_in)
document.addEventListener('DOMContentLoaded', manage_page_by_login_status());

// Login form submit
document.getElementById("login_form").addEventListener("submit", function(event) {
  event.preventDefault(); // Prevent default form submission
  
  // Get the values entered by the user
  var email = document.getElementById("LoginEmail").value;
  var password = document.getElementById("LoginPassword").value;

  // Create a data object with the values
  var data = {
    email: email,
    password: password
  };

  // Convert the data object to a JSON string
  var jsonData = JSON.stringify(data);

  login_request(jsonData).then(userData => {
    if (userData)
    {
      // Store user data on local storage
      localStorage.setItem("userData", JSON.stringify(userData))
      // Display logged in page
      document.querySelector('#not_logged_in_page').style.display = 'none';
      document.querySelector('#logged_in_page').style.display = 'block';
      // Load transaction table
      load_transactions_table_page()
    }
  })
});

// Register form submit
document.getElementById("register_form").addEventListener("submit", function(event) {
  event.preventDefault(); // Prevent default form submission

  // Get the values entered by the user
  var email = document.getElementById("RegisterEmail").value;
  var password = document.getElementById("RegisterPassword1").value;
  var password2 = document.getElementById("RegisterPassword2").value;
  var first_name = document.getElementById("RegisterFirstName").value;
  var last_name = document.getElementById("RegisterLastname").value;
  var is_manager = document.getElementById("RegisterManagerCheck").checked;
  console.log(password)
  if (password == password2){
    // Create a data object with the values

    var data = {
      "email": email,
      "password": password,
      "first_name": first_name,
      "last_name": last_name,
      "is_manager": is_manager
    };

    // Convert the data object to a JSON string
    var jsonData = JSON.stringify(data);

    register_request(jsonData)
  }
  else{
    document.getElementById("unlogged_in_error_message").textContent = "Passwords not match"; // Update error message
    document.getElementById("unlogged_in_error_message").style.display = "block"; // Show error message
  }

});

// New Transaction form submit
document.getElementById("add_transaction_form").addEventListener("submit", function(event) {
  event.preventDefault(); // Prevent default form submission
  // Get USER_DATA from localstorage
  USER_DATA = JSON.parse(localStorage.getItem("userData"))

  // Get the values entered by the user
  var department = document.getElementById("NewTransactionDepartment").value;
  var amount = document.getElementById("NewTransactionAmount").value;

  // Create a data object with the values
  var data = {
    department: department,
    amount: amount,
    created_by: USER_DATA.first_name + " " + USER_DATA.last_name
  };

  console.log(data)
  // Convert the data object to a JSON string
  var jsonData = JSON.stringify(data);

  new_transaction_request(jsonData).then(data => {
    if (data){
      // display index page
      load_transactions_table_page()
    }
  })
});

// Define alert disapper when click anywhere
document.body.addEventListener("click", function(event){
  document.getElementById("unlogged_in_alert_message").style.display = "none"
})
// Define alert disapper when click anywhere
document.body.addEventListener("click", function(event){
  document.getElementById("logged_in_alert_message").style.display = "none"
})


// Not logged in navbar click listeners
document.getElementById("register_page").addEventListener('click', function() {
  document.getElementById("unlogged_in_error_message").style.display = "none";
  document.querySelector('#login_form').style.display = 'none';
  document.querySelector('#register_form').style.display = 'block';
});
document.getElementById("login_page").addEventListener('click', function() {
  document.getElementById("unlogged_in_error_message").style.display = "none";
  document.querySelector('#login_form').style.display = 'block';
  document.querySelector('#register_form').style.display = 'none';
});
// Logged in navbar click listeners
document.getElementById("add_transaction_page").addEventListener('click', () => load_add_transaction_page());
document.getElementById("all_transactions_page").addEventListener('click', () => load_transactions_table_page());
document.getElementById("logout").addEventListener('click', () => logout_request());
