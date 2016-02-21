package teo.brujulavoz;
/*
*  Copyright (C) 2015, 2016 - Matteo Lucchelli, Cristóbal Olivencia Carrión>
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
import android.app.Activity;
import android.app.ListActivity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.speech.RecognizerIntent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;


public class MainActivity extends Activity {


    private float error;
    private String cardinal;
    private String word;


    private static final int REQUEST_CODE = 1234;
    private ListView wordsList;
    TextView tv;

    /**
     * Called with the activity is first created.
     */
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        tv = (TextView)findViewById(R.id.results);

        Button speakButton = (Button) findViewById(R.id.speakButton);

        wordsList = (ListView) findViewById(R.id.list);

        // Disable button if no recognition service is present
        PackageManager pm = getPackageManager();
        List<ResolveInfo> activities = pm.queryIntentActivities(
                new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH), 0);
        if (activities.size() == 0)
        {
            speakButton.setEnabled(false);
            speakButton.setText("Recognizer not present");
        }
    }

    /**
     * Handle the action of the button being clicked
     */
    public void speakButtonClicked(View v)
    {

        startVoiceRecognitionActivity();
    }

    /**
     * Fire an intent to start the voice recognition activity.
     */
    private void startVoiceRecognitionActivity()
    {
        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL,
                RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_PROMPT, "Voice recognition Demo...");
        startActivityForResult(intent, REQUEST_CODE);
    }

    /**
     * Handle the results from the voice recognition activity.
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        if (requestCode == REQUEST_CODE && resultCode == RESULT_OK)
        {
            // Populate the wordsList with the String values the recognition engine thought it heard
            ArrayList<String> matches = data.getStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS);
           //wordsList.setAdapter(new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1,matches));

            List <String> listnorte = new ArrayList<String>();
            for (String string : matches) {
                if(string.matches("(?i)(norte).*.([0-9])*")){
                    listnorte.add(string);
                }
                if(listnorte.size()>0)
                word=listnorte.get(0);
            }

            List <String> listsur = new ArrayList<String>();
            for (String string : matches) {
                if(string.matches("(?i)(sur).*.([0-9])*")){
                    listsur.add(string);
                }
                if(listsur.size()>0)
                word=listsur.get(0);
            }
            List <String> listoeste = new ArrayList<String>();
            for (String string : matches) {
                if(string.matches("(?i)(oeste).*.([0-9])*")){
                    listoeste.add(string);
                }
                if(listoeste.size()>0)
                word=listoeste.get(0);
            }

            List <String> listeste = new ArrayList<String>();
            for (String string : matches) {
                if(string.matches("(?i)(este).*.([0-9])*")){
                    listeste.add(string);
                }
                if(listeste.size()>0)
                word=listeste.get(0);
            }

            //word=matches.get(0);
            if (word.length()==0)
                    word = matches.get(0);

            String[] parts = word.split(" ");
            if(parts.length==2 && isParsable(parts[1])) {
                cardinal = parts[0].toLowerCase(); // Cardinal
                error = Integer.parseInt(parts[1]); // error convertido en int
                tv.setText("direccion: " + cardinal + "  error: " + error);
                if(cardinal.equals("norte")||cardinal.equals("sur")||cardinal.equals("oeste")||cardinal.equals("este")){
                    Intent brujulaIntent = new Intent(this, CompassActivity.class);
                    brujulaIntent.putExtra("cardinal", cardinal);
                    brujulaIntent.putExtra("error", error);
                    startActivity(brujulaIntent);

                }
            }

        }
        super.onActivityResult(requestCode, resultCode, data);
    }

    public static boolean isParsable(String input){
        boolean parsable = true;
        try{
            Integer.parseInt(input);
        }catch(NumberFormatException e){
            parsable = false;
        }
        return parsable;
    }




}
