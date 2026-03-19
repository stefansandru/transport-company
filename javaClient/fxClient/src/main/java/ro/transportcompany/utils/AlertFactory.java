package ro.transportcompany.utils;

import javafx.scene.control.Alert;

public class AlertFactory {
    private static AlertFactory instance = null;

    private AlertFactory() {}

    public static AlertFactory getInstance() {
        if (instance == null) {
            instance = new AlertFactory();
        }
        return instance;
    }

    public Alert createAlert(String header, String content) {
        Alert alert = new Alert(Alert.AlertType.INFORMATION);
        alert.setTitle("Task management system");
        alert.setHeaderText(header);
        alert.setContentText(content);
        return alert;
    }
}
