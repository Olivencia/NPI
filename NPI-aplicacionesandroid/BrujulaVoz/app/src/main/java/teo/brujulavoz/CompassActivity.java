package teo.brujulavoz;

import android.app.Activity;
import android.graphics.Color;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.view.animation.Animation;
import android.view.animation.RotateAnimation;
import android.widget.ImageView;
import android.widget.TextView;

/**
 * Created by Teo on 09/02/2016.
 */
public class CompassActivity extends Activity implements SensorEventListener {

    private String cardinal;
    float error;

    private float currentDegree = 0f;

    private TextView actAngle,bravo;
    private ImageView imgCompass;
    private SensorManager mSensorManager;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_compass);


        imgCompass = (ImageView) findViewById(R.id.Compass);
        actAngle = (TextView) findViewById(R.id.actAngle);
        bravo=(TextView) findViewById(R.id.bravo);

        Bundle extras = getIntent().getExtras();
        if (extras != null) {
            cardinal = extras.getString("cardinal");
            error = extras.getFloat("error");
        }

        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
    }


    @SuppressWarnings("deprecation")
    @Override
    protected void onResume() {
        super.onResume();

        mSensorManager.registerListener(this, mSensorManager.getDefaultSensor(Sensor.TYPE_ORIENTATION),
                SensorManager.SENSOR_DELAY_GAME);
    }

    @Override
    protected void onPause() {
        super.onPause();

        mSensorManager.unregisterListener(this);
    }

    @Override
    public void onSensorChanged(SensorEvent event) {

        // valor del sensor
        float degree = Math.round(event.values[0]);

        actAngle.setText("Heading: " + Float.toString(degree) + " degrees");

        //  rotation animation
        RotateAnimation ra = new RotateAnimation(
                currentDegree,
                -degree,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF,
                0.5f);

        // how long the animation will take place
        ra.setDuration(50);

        // set the animation after the end of the reservation status
        ra.setFillAfter(true);

        // Start the animation
        imgCompass.startAnimation(ra);
        currentDegree = -degree;

        if( cardinal.equals("norte")){
            if(degree < error || degree > 360.0 - error ){
                bravo.setBackgroundColor(Color.GREEN);
                bravo.setText("Direccion Correcta!");
            }else{
                bravo.setBackgroundColor(Color.RED);
                bravo.setText("Direccion Incorrecta!");
            }
        }
        if( cardinal.equals("sur")){
            if(degree > 180.0 - error && degree < 180.0 + error ){
                bravo.setBackgroundColor(Color.GREEN);
                bravo.setText("Direccion Correcta!");
            }else{
                bravo.setBackgroundColor(Color.RED);
                bravo.setText("Direccion Incorrecta!");
            }
        }
        if (cardinal.equals("oeste")) {
            if (degree > 270.0 - error && degree < 270.0 + error){
                bravo.setBackgroundColor(Color.GREEN);
                bravo.setText("Direccion Correcta!");
            }else{
                bravo.setBackgroundColor(Color.RED);
                bravo.setText("Direccion Incorrecta!");
            }
        }
        if (cardinal.equals("este")) {
            if (degree > 90.0 - error && degree < 90.0 + error) {
                bravo.setBackgroundColor(Color.GREEN);
                bravo.setText("Direccion Correcta!");
            } else {
                bravo.setBackgroundColor(Color.RED);
                bravo.setText("Direccion Incorrecta!");

            }
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

}
