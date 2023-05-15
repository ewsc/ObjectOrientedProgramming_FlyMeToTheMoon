package com.alamov.flymetothemoon;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Scene;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;

import java.io.IOException;

public class GameField extends Application {
    @Override
    public void start(Stage stage) throws IOException {
        StackPane root = new StackPane();
        root.setId("pane");
        Scene scene = new Scene(root, 256, 272);
        scene.getStylesheets().addAll(this.getClass().getResource("style.css").toExternalForm());
        stage.setTitle("Fly me to the Moon");
        stage.setScene(scene);
        stage.show();
    }

    public static void main(String[] args) {
        launch();
    }
}