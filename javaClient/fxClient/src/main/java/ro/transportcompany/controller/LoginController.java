package ro.transportcompany.controller;

import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.Node;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.control.Label;
import javafx.scene.control.TextField;
import javafx.stage.Stage;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import ro.transportcompany.Employee;
import ro.transportcompany.IServices;
import ro.transportcompany.ServicesException;
import ro.transportcompany.utils.AlertFactory;

public class LoginController {
    private IServices server;
    private MainAppController mainAppController;
    private Employee currentEmployee;

    private static final Logger logger = LogManager.getLogger(LoginController.class);

    @FXML
    private TextField usernameField;

    @FXML
    private TextField passwordField;

    @FXML
    private Label errorLabel;

    private Parent root;

    public LoginController(IServices server) {
        logger.trace("Entering LoginController constructor");
        this.server = server;
    }

    public void setServer(IServices server) {
        logger.trace("Entering setServer");
        this.server = server;
    }

    public void setMainAppController(MainAppController mainAppController) {
        logger.trace("Entering setMainAppController");
        this.mainAppController = mainAppController;
    }

    public void setParent(Parent root) {
        this.root = root;
    }

    @FXML
    private void handleLogin(ActionEvent event) {
        logger.trace("Entering handleLogin");
        String username = usernameField.getText();
        String password = passwordField.getText();

        try {
            currentEmployee = server.login(username, password, mainAppController);
            logger.info("User {} logged in successfully", username);
            mainAppController.setCurrentEmployee(currentEmployee);
            logger.info("Current employee set to {}", currentEmployee.getUsername());

            mainAppController.loadTrips();

            Stage mainAppStage = new Stage();
            mainAppStage.setTitle("Main App - " + currentEmployee.getUsername());
            mainAppStage.setScene(new Scene(root, 700, 800));
            logger.info("Main app scene set");
            mainAppStage.show();

            ((Node) (event.getSource())).getScene().getWindow().hide();
        } catch (ServicesException e) {
            logger.error("Login failed: {}", e.getMessage());
            AlertFactory.getInstance().createAlert("Login failed", e.getMessage()).showAndWait();
        } catch (Exception e) {
            logger.error("Error during login: {}", e.getMessage());
            errorLabel.setText("An error occurred: " + e.getMessage());
        }
    }
}