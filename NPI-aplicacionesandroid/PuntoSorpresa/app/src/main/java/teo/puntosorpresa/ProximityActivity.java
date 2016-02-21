package teo.puntosorpresa;


import android.app.Activity;
import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.media.MediaPlayer;
import android.os.Bundle;
import android.widget.TextView;

public class ProximityActivity extends Activity {
    /**
     * Called when the activity is first created.
     */

    private TextView txtProximitySensor, txtMaxProximity, txtReadingProximity;
    private SensorManager mSensorManager;
    private Sensor mProximitySensor;
    private MediaPlayer hit;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_proximity);


        hit = MediaPlayer.create(this,R.raw.bell);

        mSensorManager = (SensorManager) getSystemService(
                Context.SENSOR_SERVICE);
        mProximitySensor = mSensorManager.getDefaultSensor(
                Sensor.TYPE_PROXIMITY);

        if (mProximitySensor == null) {
            txtProximitySensor.setText("Proximity sensor is not present!");

        } else {
            /*txtProximitySensor.setText("Sensor present with name:" + " " + mProximitySensor.getName());
            txtMaxProximity.setText("Maximum Range: "
                    + String.valueOf(mProximitySensor.getMaximumRange()));*/
            mSensorManager.registerListener(proximitySensorEventListener,
                    mProximitySensor,
                    SensorManager.SENSOR_DELAY_NORMAL);
        }
    }

    SensorEventListener proximitySensorEventListener
            = new SensorEventListener() {
        @Override
        public void onAccuracyChanged(Sensor sensor, int accuracy) {
            // TODO Auto-generated method stub
        }

        @Override
        public void onSensorChanged(SensorEvent event) {
            // TODO Auto-generated method stub
            if (event.sensor.getType() == Sensor.TYPE_PROXIMITY
                    && event.values[0]==0) {
                hit.start();
            }
        }
    };

    @Override
    protected void onResume() {
        super.onResume();
        mSensorManager.registerListener(proximitySensorEventListener,
                mProximitySensor,
                SensorManager.SENSOR_DELAY_NORMAL);
    }

    @Override
    protected void onPause() {
        super.onPause();
        mSensorManager.unregisterListener(proximitySensorEventListener);
    }
}