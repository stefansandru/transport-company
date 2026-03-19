package ro.transportcompany;

import javafx.scene.Parent;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Scene;
import javafx.stage.Stage;
import ro.transportcompany.controller.LoginController;
import ro.transportcompany.controller.MainAppController;
import ro.transportcompany.proto.LoginReply;
import ro.transportcompany.proto.TransportCompanyGrpc;
import ro.transportcompany.proto.LoginRequest;

import io.grpc.ManagedChannel;

import java.io.File;
import java.io.IOException;
import java.util.Properties;

import io.grpc.ManagedChannelBuilder;


public class StartJsonFXClient extends Application {
    private Stage primaryStage;

    private static int defaultServerPort = 5000;
    private static String defaultServer = "localhost";

    private static final Logger logger = LogManager.getLogger(StartJsonFXClient.class);

    @Override
    public void start(Stage primaryStage) throws Exception {
        Properties clientProps = new Properties();

        try {
            clientProps.load(StartJsonFXClient.class.getResourceAsStream("/client.properties"));
            logger.info("Client properties set {} ",clientProps);
        } catch (IOException e) {
            logger.error("Cannot find client.properties {}", String.valueOf(e));
            return;
        }
        String serverIP = clientProps.getProperty("server.host", defaultServer);
        int serverPort = defaultServerPort;

        try {
            serverPort = Integer.parseInt(clientProps.getProperty("server.port"));
        } catch (NumberFormatException ex) {
            logger.error("Wrong port number {}", ex.getMessage());
        }
        logger.info("Using server IP {}", serverIP);
        logger.info("Using server port {}", serverPort);

        ManagedChannel channel = ManagedChannelBuilder
                .forAddress(serverIP, serverPort)
                .usePlaintext()
                .build();
        TransportCompanyGrpc.TransportCompanyBlockingStub grpcStub =
                TransportCompanyGrpc.newBlockingStub(channel);

        IServices server = new GrpcServicesProxy(grpcStub);

        logger.info("Login view start loading");

        FXMLLoader loginLoader = new FXMLLoader(getClass().getClassLoader().getResource("view/login-view.fxml"));
        loginLoader.setControllerFactory(param -> new LoginController(server));
        Parent root = loginLoader.load();
        LoginController loginController = loginLoader.getController();
        loginController.setServer(server);

        logger.info("Login view loaded");

        FXMLLoader mainAppLoader = new FXMLLoader(getClass().getClassLoader().getResource("view/main-app-view.fxml"));
        mainAppLoader.setControllerFactory(param -> new MainAppController());
        Parent mainAppRoot = mainAppLoader.load();
        MainAppController MainAppController = mainAppLoader.getController();
        MainAppController.setServer(server);

        loginController.setMainAppController(MainAppController);
        loginController.setParent(mainAppRoot);

        primaryStage.setTitle("Login");
        primaryStage.setScene(new Scene(root, 300, 250));
        primaryStage.show();
    }

    public static void main(String[] args) throws ServicesException {
        launch(args);
    }
}