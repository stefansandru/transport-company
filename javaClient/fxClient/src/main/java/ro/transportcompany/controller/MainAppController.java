package ro.transportcompany.controller;

import javafx.application.Platform;
import javafx.collections.FXCollections;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.fxml.Initializable;
import javafx.scene.Node;
import javafx.scene.control.*;
import javafx.scene.control.cell.PropertyValueFactory;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import ro.transportcompany.*;
import ro.transportcompany.utils.AlertFactory;

import java.net.URL;
import java.time.LocalDate;
import java.time.LocalTime;
import java.util.List;
import java.util.ResourceBundle;

public class MainAppController implements Initializable, IObserver {
    private IServices server;
    private Employee currentEmployee;
    private Trip tripToReserve;

    private static final Logger logger = LogManager.getLogger(ro.transportcompany.controller.MainAppController.class);

    @FXML
    private TableView<Trip> tripsTable;

    @FXML
    private TableColumn<Trip, String> destinationColumn;

    @FXML
    private TableColumn<Trip, LocalDate> dateColumn;

    @FXML
    private TableColumn<Trip, LocalTime> timeColumn;

    @FXML
    private TableColumn<Trip, Integer> seatsColumn;

    @FXML
    private TextField searchDestinationField;

    @FXML
    private DatePicker searchDateField;

    @FXML
    private TextField searchTimeField;

    @FXML
    private TableView<Seat> seatsTable;

    @FXML
    private TableColumn<Seat, Integer> seatNumberColumn;

    @FXML
    private TableColumn<Seat, String> clientNameColumn;

    @FXML
    private TextField clientNameField;

    @FXML
    private TextField seatNumbersField;

    @FXML
    private Button searchButton;

    @FXML
    private Button reserveButton;

    @Override
    public void initialize(URL location, ResourceBundle resources) {
        initTripsTable();
        initSeatsTable();

        tripsTable.getSelectionModel().selectedItemProperty().addListener((obs, oldTrip, selectedTrip) -> {
            if (selectedTrip != null) {
                searchDestinationField.setText(selectedTrip.getDestination().getName());
                searchDateField.setValue(selectedTrip.getDepartureDate());
                searchTimeField.setText(selectedTrip.getDepartureTime().toString());
            }
        });
    }

    public void seatsReserved() {
        Platform.runLater(()-> {
            loadTrips();
            loadSeats();
        });
    }

    public void setServer(IServices server) {
        this.server = server;
    }

    public void setCurrentEmployee(Employee employee) {
        this.currentEmployee = employee;
    }

    public void loadTrips() {
        try {
            List<Trip> trips = server.getAllTrips();
            tripsTable.setItems(FXCollections.observableArrayList(trips));
        }
        catch (Exception e) {
            logger.error("Error loading trips: {}", e.getMessage());
        }
    }

    private void initTripsTable() {
        destinationColumn.setCellValueFactory(new PropertyValueFactory<>("destination"));
        dateColumn.setCellValueFactory(new PropertyValueFactory<>("departureDate"));
        timeColumn.setCellValueFactory(new PropertyValueFactory<>("departureTime"));
        seatsColumn.setCellValueFactory(new PropertyValueFactory<>("availableSeats"));

        tripsTable.setColumnResizePolicy(TableView.CONSTRAINED_RESIZE_POLICY_FLEX_LAST_COLUMN);
    }

    private void initSeatsTable() {
        seatNumberColumn.setCellValueFactory(new PropertyValueFactory<>("seatNumber"));
        clientNameColumn.setCellValueFactory(new PropertyValueFactory<>("clientName"));

        seatsTable.setColumnResizePolicy(TableView.CONSTRAINED_RESIZE_POLICY_FLEX_LAST_COLUMN);
    }

    @FXML
    private void onSearchButtonClick(ActionEvent event) {
        try {
            loadSeats();
        } catch (Exception e) {
            AlertFactory.getInstance().createAlert("Error", "Invalid input format: " + e.getMessage()).showAndWait();
        }
    }

    private void loadSeats() {
        try {
            String destination = searchDestinationField.getText();
            if (destination.isEmpty()) {
                AlertFactory.getInstance().createAlert("Error", "Destination cannot be empty!").showAndWait();
                return;
            }
            LocalDate date = searchDateField.getValue();
            if (date == null) {
                AlertFactory.getInstance().createAlert("Error", "Date cannot be empty!").showAndWait();
                return;
            }
            LocalTime time = LocalTime.parse(searchTimeField.getText());

            Trip newTrip = server.getTrip(destination, date, time);
            if (newTrip == null) {
                AlertFactory.getInstance().createAlert("Error", "This trip don't exists!").showAndWait();
                return;
            }
            tripToReserve = newTrip;
            List<Seat> seats = server.searchTripSeats(
                    tripToReserve.getDestination().getName(),
                    tripToReserve.getDepartureDate(),
                    tripToReserve.getDepartureTime()
            );
            seatsTable.setItems(FXCollections.observableArrayList(seats));
        } catch (ServicesException e) {
            logger.error("Error loading seats: {}", e.getMessage());
        }
    }

    @FXML
    private void onReserveButtonClick(ActionEvent event) {
        try {
            String clientName = clientNameField.getText();
            List<Integer> seatNumbers = List.of(seatNumbersField.getText().split(","))
                    .stream()
                    .map(Integer::parseInt)
                    .toList();

            server.reserveSeats(clientName, seatNumbers, tripToReserve, currentEmployee);
            AlertFactory.getInstance().createAlert("Success", "Seats reserved successfully!").showAndWait();
            loadTrips();
            loadSeats();
        } catch (ServicesException e) {
            logger.error("Error reserving seats cibidusoewoiiub");
            AlertFactory.getInstance().createAlert("Error", e.getMessage()).showAndWait();
        }
    }

    public void onLogoutButtonClick(ActionEvent actionEvent) {
        try {
            server.logout(currentEmployee);
            AlertFactory.getInstance().createAlert("Success", "Logged out successfully!").showAndWait();
        } catch (ServicesException e) {
            logger.error("Error logging out: {}", e.getMessage());
        }
        ((Node) (actionEvent.getSource())).getScene().getWindow().hide();
    }
}